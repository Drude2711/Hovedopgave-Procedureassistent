import * as React from "react";
import { ISource } from "../../Api/Models/ISource";
import styles from "./Chat.module.scss";

export interface ISourceListProps {
  sources: ISource[];
}

export const SourceList: React.FunctionComponent<ISourceListProps> = (props: ISourceListProps) => {
  const { sources } = props;
  if (sources.length === 0) {
    return null;
  }

  return (
    <div className={styles.sources}>
      <span className={styles.sourcesLabel}>Kilder:</span>
      {sources.map((source, index) => (
        <span key={index} className={styles.source}>
          {source.url
            ? <a href={source.url} target="_blank" rel="noreferrer">{source.title}</a>
            : source.title}
        </span>
      ))}
    </div>
  );
};
